import axios from 'axios';
import { fetchEventSource, EventSourceMessage } from '@microsoft/fetch-event-source';

const API_BASE_URL = process.env.REACT_APP_BACKEND_URL;

export enum RateLimitErrorCode {
  GlobalLimitExceeded = 0,
  SessionLimitExceeded = 1
}

export interface RateLimitErrorResponse {
  code: RateLimitErrorCode;
  message: string;
}

export class RateLimitError extends Error {
  constructor(public readonly response: RateLimitErrorResponse) {
    super(response.message);
    this.name = 'RateLimitError';
  }
}

export interface ChatRequest {
  conversationId?: string;
  message: string;
  employeeId: string;
  signatures: { toolId: string; content: string }[];
}

export interface ChatResponse {
  conversationId: string;
  answer: string;
  followups: string[];
  generatedAt: string;
  documentsToSign: SignatureDocumentRequest[];
}

export interface SignatureDocumentRequest {
  toolId: string;
  documentId: string;
  title: string;
  content: string;
  version: number;
}

export interface SignDocumentRequest {
  conversationId: string;
  employeeId: string;
  toolId: string;
  documentId: string;
  confirmed: boolean;
  signatureBlob?: string;
}

export interface ChatMessage {
  id: string;
  text: string;
  isUser: boolean;
  timestamp: string;
}

export interface ChatHistoryResponse {
  conversationId: string;
  messages: ChatMessage[];
  employeeId: string;
}

export interface EmployeeDropdown {
  id: string;
  name: string;
  department: string;
  jobTitle: string;
  email: string;
}

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const hrApi = {

  chat: (
    request: ChatRequest,
    onChunk: (chunk: string) => void
  ): Promise<ChatResponse> => {
    const ctrl = new AbortController();
    return new Promise<ChatResponse>((resolve, reject) => {
      fetchEventSource(`${API_BASE_URL}/api/HumanResourcesAgent/chat`, {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
        signal: ctrl.signal, 
        openWhenHidden: true,
        async onopen(response) {
          if (response.ok) return;
          if (response.status === 429) {
            const errorData: RateLimitErrorResponse = await response.json();
            ctrl.abort();
            reject(new RateLimitError(errorData));
            return;
          }
          ctrl.abort();
          reject(new Error(`HTTP ${response.status}`));
        },
        onmessage(ev: EventSourceMessage) {
          if (ev.event === 'final') {
            resolve(JSON.parse(ev.data));
          } else {
            onChunk(ev.data);
          }
        },
        onerror(err: any) {
          ctrl.abort();
          throw err;
        },
      });
    });
  },

  signDocument: (request: SignDocumentRequest): Promise<any> =>
    api.post('/api/HumanResourcesAgent/sign-document', request).then(response => response.data),

  getChatHistory: (employeeId: string): Promise<ChatHistoryResponse> =>
    api.get(`/api/HumanResourcesAgent/chat/today/${employeeId}`).then(response => response.data),

  getEmployeesForDropdown: (): Promise<EmployeeDropdown[]> =>
    api.get('/api/HumanResourcesAgent/employees/dropdown').then(response => response.data),
};