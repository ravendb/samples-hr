using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.AI;
using SamplesHR.Backend.Application.Exception;
using SamplesHR.Backend.Application.Usage;
using SamplesHR.Backend.Models;
using SamplesHR.Backend.Models.RavenDBAiAgent;

namespace SamplesHR.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HumanResourcesAgentController(
        IDocumentStore documentStore,
        ILogger<HumanResourcesAgentController> logger,
        IOptions<JsonOptions> jsonOptions,
        SessionApiUsageLimiter usageLimiter,
        GlobalApiUsageLimiter globalUsageLimiter,
        SessionApiUsageTracker usageTracker,
        GlobalApiUsageTracker globalUsageTracker)
        : ControllerBase
    {
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.JsonSerializerOptions;

        [HttpPost("chat")]
        public async Task Chat([FromBody] ChatRequest request)
        {
            try
            {
                
                await globalUsageLimiter.EnsureAllowedAsync();

                var sessionId = HttpContext.Items["SessionId"]?.ToString()
                                ?? throw new Exception("SessionId missing from middleware.");

                await usageLimiter.EnsureAllowedAsync(sessionId);
            }
            catch (GlobalRateLimitExceededException ex)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    code = LimitExceededErrorCode.GlobalLimitExceeded,
                    message = ex.Message
                });
                return;
            }
            catch (SessionRateLimitExceededException ex)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    code = LimitExceededErrorCode.SessionLimitExceeded,
                    message = ex.Message
                });
                return;
            }
            
            try
            {
                var documentsToSign = new List<SignatureDocumentRequest>();
                var conversationId = request.ConversationId ?? "hr/" + request.EmployeeId + "/" + DateTime.Today.ToString("yyyy-MM-dd");
                var conversation = documentStore.AI.Conversation(
                    agentId: "hr-assistant", conversationId,
                    new AiConversationCreationOptions
                    {
                        Parameters = new Dictionary<string, object>
                        {
                            ["employeeId"] = request.EmployeeId
                        },
                        ExpirationInSec = 60 * 60 * 24 * 30 // 30 days
                    });

                conversation.Handle<RaiseIssueArgs>("RaiseIssue", async (args) =>
                {
                    using var session = documentStore.OpenAsyncSession();
                    var issue = new HrIssue
                    {
                        EmployeeId = request.EmployeeId,
                        Title = args.Title,
                        Description = args.Description,
                        Category = args.Category,
                        Priority = args.Priority,
                        SubmittedDate = DateTime.UtcNow,
                        Status = "Open"
                    };
                    await session.StoreAsync(issue);
                    await session.SaveChangesAsync();

                    return "Raised issue: " + issue.Id;
                });

                conversation.Receive<SignDocumentArgs>("SignDocument", async (req, args) =>
                {
                    using var session = documentStore.OpenAsyncSession();
                    var document = await session.LoadAsync<SignatureDocument>(
                                            args.Document
                                        );
                    documentsToSign.Add(new SignatureDocumentRequest
                    {
                        ToolId = req.ToolId,
                        DocumentId = document.Id,
                        Title = document.Title,
                        Content = document.Content,
                        Version = document.Version
                    });
                });

                foreach (var signature in request.Signatures ?? [])
                {
                    conversation.AddActionResponse(signature.ToolId, signature.Content);
                }
                if (string.IsNullOrWhiteSpace(request.Message) is false)
                {
                    conversation.SetUserPrompt(request.Message);
                }

                Response.Headers.ContentType = "text/event-stream";
                await using var writer = new StreamWriter(Response.Body);
                var result = await conversation.StreamAsync<Reply>("Answer", async chunk =>
                {
                    await writer.WriteAsync("data: ");
                    await writer.WriteAsync(JsonSerializer.Serialize(chunk, _jsonOptions));
                    await writer.WriteAsync("\n\n");
                    await writer.FlushAsync();
                });

                if (result is not null)
                {
                    await usageTracker.TrackAsync(HttpContext, result.Usage);
                    await globalUsageTracker.TrackGlobalAsync(HttpContext, result.Usage);
                }

                // Send the final result as a custom event
                var finalResponse = new ChatResponse
                {
                    ConversationId = conversation.Id,
                    Answer = result.Answer?.Answer,
                    Followups = result.Answer?.Followups ?? [],
                    GeneratedAt = DateTime.UtcNow,
                    DocumentsToSign = documentsToSign
                };
                await writer.WriteAsync($"event: final\ndata: ");
                await writer.WriteAsync(JsonSerializer.Serialize(finalResponse, _jsonOptions));
                await writer.WriteAsync("\n\n");
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in chat endpoint for employee {EmployeeId}", request.EmployeeId);

                if (Response.HasStarted == false)
                {
                    Response.Headers.ContentType = "text/event-stream";
                }

                await using var writer = new StreamWriter(Response.Body);

                var errorResponse = new ChatResponse
                {
                    ConversationId = request.ConversationId ?? "",
                    Answer = "I'm sorry, I'm having trouble connecting right now. Please try again in a moment, or contact IT support if the problem persists.",
                    Followups = [],
                    GeneratedAt = DateTime.UtcNow,
                    DocumentsToSign = []
                };

                await writer.WriteAsync($"event: error\ndata: ");
                await writer.WriteAsync(JsonSerializer.Serialize(errorResponse, _jsonOptions));
                await writer.WriteAsync("\n\n");
                await writer.FlushAsync();
            }
        }

        [HttpGet("chat/today/{*employeeId}")]
        public async Task<ActionResult<ChatHistoryResponse>> GetChatHistory(string employeeId)
        {
            var conversationId = "hr/" + employeeId + "/" + DateTime.Today.ToString("yyyy-MM-dd");
            using var session = documentStore.OpenAsyncSession();
            var chat = await session.LoadAsync<Conversation>(conversationId);

            var messages = chat?.Messages ?? [];

            return Ok(new ChatHistoryResponse
            {
                ConversationId = conversationId,
                Messages = messages
                    // skip tool calls, system prompt, etc
                    .Where(m => m.Role == "user" || m.Role == "assistant")
                    // skip AI Agent Parameters
                    .Where(m => m.Content is not null && !m.Content.StartsWith("AI Agent Parameters"))
                    .Select((m, i) => new ChatMessage
                    {
                        Id = conversationId + "#" + i,
                        Text = m.Role switch
                        {
                            "assistant" when m.Content.StartsWith('{') => JsonSerializer.Deserialize<Reply>(m.Content)!.Answer,
                            _ => m.Content,
                        },
                        IsUser = m.Role == "user",
                        Timestamp = m.Date
                    }).ToArray(),
                EmployeeId = employeeId
            });
        }

        [HttpGet("employees/dropdown")]
        public async Task<ActionResult<List<object>>> GetEmployeesForDropdown()
        {
            using var session = documentStore.OpenAsyncSession();
            var employees = await session.Query<Employee>()
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Department,
                    e.JobTitle,
                    e.Email
                })
                .ToListAsync();
            return Ok(employees);
        }

        [HttpGet("signature-documents")]
        public async Task<ActionResult<List<SignatureDocument>>> GetSignatureDocuments()
        {
            using var session = documentStore.OpenAsyncSession();
            var documents = await session.Query<SignatureDocument>()
                .Where(d => d.IsActive)
                .OrderBy(d => d.Title)
                .ToListAsync();
            return Ok(documents);
        }

        [HttpGet("signature-documents/{documentId}")]
        public async Task<ActionResult<SignatureDocument>> GetSignatureDocument(string documentId)
        {
            using var session = documentStore.OpenAsyncSession();
            var document = await session.LoadAsync<SignatureDocument>(documentId);
            if (document == null)
            {
                return NotFound();
            }
            return Ok(document);
        }

        [HttpGet("employees/{employeeId}/signed-documents")]
        public async Task<ActionResult<List<SignedDocument>>> GetEmployeeSignedDocuments(string employeeId)
        {
            using var session = documentStore.OpenAsyncSession();
            var employee = await session.LoadAsync<Employee>($"employees/{employeeId}");
            if (employee == null)
            {
                return NotFound("Employee not found");
            }
            return Ok(employee.SignedDocuments);
        }

        [HttpPost("sign-document")]
        public async Task<ActionResult<ChatResponse>> SignDocument([FromBody] SignDocumentRequest request)
        {
            using var session = documentStore.OpenAsyncSession();
            var employee = await session.LoadAsync<Employee>(request.EmployeeId);
            var document = await session.LoadAsync<SignatureDocument>(request.DocumentId);

            var attachmentName = request.DocumentId + "-signature.png";

            // Extract base64 data from data URL (format: data:image/png;base64,<base64data>)
            var base64Data = request.SignatureBlob ?? "";
            if (base64Data.StartsWith("data:"))
            {
                var commaIndex = base64Data.IndexOf(',');
                if (commaIndex > 0)
                {
                    base64Data = base64Data.Substring(commaIndex + 1);
                }
            }

            session.Advanced.Attachments.Store(
                employee,
                attachmentName,
                new MemoryStream(Convert.FromBase64String(base64Data)),
                "image/png"
            );

            var signedDocument = new SignedDocument
            {
                DocumentId = document.Id,
                DocumentTitle = document.Title,
                DocumentVersion = document.Version,
                SignedDate = DateTime.UtcNow,
                SignedBy = employee.Name,
                SignatureAttachmentName = attachmentName,
                SignatureMethod = "Digital"
            };

            employee.SignedDocuments.Add(signedDocument);
            await session.StoreAsync(employee);
            await session.SaveChangesAsync();

            return Ok(new
            {
                Id = session.Advanced.GetDocumentId(signedDocument)
            });
        }
    }
}