using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations.AI.Agents;
using SamplesHR.Backend.Models.RavenDBAiAgent;

namespace SamplesHR.Backend.Infrastructure.RavenDB;

public static class HumanResourcesAgentCreator
{
    public const string AgentIdentifier = "hr-assistant";

    public const string ConnectionStringName = "Human Resources' AI Model";

    public const string EmployeeIdParameter = "employeeId";

    public const string RaiseIssueAction = "RaiseIssue";
    public const string SignDocumentAction = "SignDocument";
    public const string ReportBusinessTripExpenseAction = "ReportBusinessTripExpense";

    public const string ExpenseAgentIdentifier = "expense-manager";

    public static Task CreateExpenseAgent(IDocumentStore store)
    {
        return store.AI.CreateAgentAsync(
            new AiAgentConfiguration
            {
                Name = "Business Trip Expense Manager",
                Identifier = ExpenseAgentIdentifier,
                ConnectionStringName = ConnectionStringName,
                Parameters = [new AiAgentParameter(EmployeeIdParameter, "Employee ID; answer only for this employee")],
                SystemPrompt = @"You are a business trip expense assistant.

When a bill or receipt image is attached to the conversation, analyze it and extract:
- Date of the expense
- Vendor / merchant name
- Total amount
- Category (Transportation, Accommodation, Meals, Flight, Conference, Other)

Present a clear summary of the extracted bill details to the user.
Then **always** ask the user explicitly: ""Would you like to report this bill as a business trip expense?""
Wait for the user's confirmation before calling ReportBusinessTripExpense.
Do NOT call ReportBusinessTripExpense unless the user explicitly confirms (e.g. ""yes"", ""confirm"", ""go ahead"", ""report it"").

When the user asks about their monthly expenses, use GetMonthlyBusinessTripExpenses to retrieve the data, then provide:
- A table listing each bill with date, vendor, category, and amount
- The total sum of all expenses
- A breakdown by category

You can answer in markdown format, make sure to use ticks (`) whenever you discuss identifiers.
Answer only for the current employee.
",
                SampleObject = JsonConvert.SerializeObject(new Reply
                {
                    Answer = "Detailed answer to query",
                    Followups = ["Likely follow-ups"],
                }),
                Queries =
                [
                    new AiAgentToolQuery
                    {
                        Name = "GetMonthlyBusinessTripExpenses",
                        Description = "Retrieve employee's business trip expenses within a given month (provide first and last day of the month)",
                        Query = $@"
                    from BusinessTripBills
                    where EmployeeId = ${EmployeeIdParameter}
                        and ExpenseDate between $monthStart and $monthEnd
                    order by ExpenseDate desc",
                        ParametersSampleObject = "{\"monthStart\": \"yyyy-MM-dd\", \"monthEnd\": \"yyyy-MM-dd\"}"
                    },
                ],
                Actions =
                [
                    new AiAgentToolAction
                    {
                        Name = ReportBusinessTripExpenseAction,
                        Description = "Reports a business trip expense extracted from a bill/receipt image. Only call this AFTER the user explicitly confirms they want to report it.",
                        ParametersSampleObject = JsonConvert.SerializeObject(new ReportBusinessTripExpenseArgs
                        {
                            Vendor = "Merchant or vendor name from the bill",
                            Category = "Transportation | Accommodation | Meals | Flight | Conference | Other",
                            Amount = 0.00m,
                            Currency = "USD",
                            ExpenseDate = "yyyy-MM-dd",
                            Description = "Brief description of the expense"
                        })
                    },
                ]
            });
    }

    public static Task Create(IDocumentStore store)
    {
        return store.AI.CreateAgentAsync(
            new AiAgentConfiguration
            {
                Name = "HR Assistant",
                Identifier = AgentIdentifier,
                ConnectionStringName = ConnectionStringName,
                SystemPrompt = @"You are an HR assistant.
Provide info on benefits, policies, and departments.
Be professional and cheery.

You can answer in markdown format, make sure to use ticks (`) whenever you discuss identifiers.
Do not suggest actions that are not explicitly allowed by the tools available to you.
Do NOT discuss non-HR topics. Answer only for the current employee.
",
                Parameters = [new AiAgentParameter(EmployeeIdParameter, "Employee ID; answer only for this employee")],
                SampleObject = JsonConvert.SerializeObject(new Reply
                {
                    Answer = "Detailed answer to query",
                    Followups = ["Likely follow-ups"],
                }),
                SubAgents =
                [
                    new AiAgentToolSubAgent
                    {
                        Identifier = ExpenseAgentIdentifier,
                        Description = "Manages business trip expenses: analyzing receipts/bills, reporting expenses, and retrieving monthly expense summaries."
                    }
                ],
                Queries =
                [
                    new AiAgentToolQuery
                    {
                        Name = "GetEmployeeInfo",
                        Description = "Retrieve employee details",
                        Query = $"from Employees where id() = ${EmployeeIdParameter}",
                        ParametersSampleObject = "{}"
                    },
                    new AiAgentToolQuery
                    {
                        Name = "GetVacations",
                        Description = "Retrieve recent employee vacation details",
                        Query = $@"
                    from VacationRequests
                    where EmployeeId = ${EmployeeIdParameter}
                    order by SubmittedDate desc
                    limit 5",
                        ParametersSampleObject = "{}"
                    },
                    new AiAgentToolQuery
                    {
                        Name = "GetPayStubs",
                        Description = "Retrieve employee's paystubs within a given date range",
                        Query = $@"
                    from PayStubs
                    where EmployeeId = ${EmployeeIdParameter}
                        and PayDate between $startDate and $endDate
                    order by PayDate desc
                    select PayPeriodStart, PayPeriodEnd, PayDate, GrossPay, NetPay,
                            Earnings, Deductions, Taxes, YearToDateGross, YearToDateNet,
                            PayPeriodNumber, PayFrequency
                    limit 5",
                        ParametersSampleObject = "{\"startDate\": \"yyyy-MM-dd\", \"endDate\": \"yyyy-MM-dd\"}"
                    },
                    new AiAgentToolQuery
                    {
                        Name = "FindIssues",
                        Description = "Semantic search for employee's issues",
                        Query = $@"
                    from HRIssues
                    where EmployeeId = ${EmployeeIdParameter}
                        and (vector.search(embedding.text(Title), $query) or vector.search(embedding.text(Description), $query))
                    order by SubmittedDate desc
                    limit 5",
                        ParametersSampleObject = "{\"query\": [\"query terms to find matching issue\"]}"
                    },
                    new AiAgentToolQuery
                    {
                        Name = "FindPolicies",
                        Description = "Semantic search for employer's policies",
                        Query = @"
                    from HRPolicies
                    where (vector.search(embedding.text(Title), $query) or vector.search(embedding.text(Content), $query))
                    limit 5",
                        ParametersSampleObject = "{\"query\": [\"query terms to find matching issue\"]}"
                    },
                    new AiAgentToolQuery
                    {
                        Name = "FindDocumentsToSign",
                        Description = "Semantic search for documents that need to be signed by the employee",
                        Query = @"
                    from SignatureDocuments
                    where vector.search(embedding.text(Title), $query)
                    select id(), Title
                    limit 5",
                        ParametersSampleObject = "{\"query\": [\"query terms to find matching document\"]}"
                    },
                ],
                Actions =
                [
                    new AiAgentToolAction
                    {
                        Name = RaiseIssueAction,
                        Description = "Raise a new HR issue for the employee (full details)",
                        ParametersSampleObject = JsonConvert.SerializeObject(new RaiseIssueArgs
                        {
                            Title = "Clear & short title describing the issue",
                            Category = "Payroll | Facilities | Onboarding | Benefits",
                            Description = "Full description, with all relevant context",
                            Priority = "Low | Medium | High | Critical"
                        })
                    },
                    new AiAgentToolAction
                    {
                        Name = SignDocumentAction,
                        Description = "Asks the employee to sign a document",
                        ParametersSampleObject = JsonConvert.SerializeObject(new SignDocumentArgs
                        {
                            Document = "unique-document-id (take from FindDocumentsToSign) ",
                        })
                    },
                ]
            });
    }
}