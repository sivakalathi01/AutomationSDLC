import { create } from 'zustand';

interface AppState {
  currentRunId: string | null;
  approvalMode: 'manual' | 'hybrid' | 'auto';
  setCurrentRunId: (runId: string) => void;
  setApprovalMode: (mode: 'manual' | 'hybrid' | 'auto') => void;
}

export const useAppStore = create<AppState>((set) => ({
  currentRunId: null,
  approvalMode: 'hybrid',
  setCurrentRunId: (runId) => set({ currentRunId: runId }),
  setApprovalMode: (mode) => set({ approvalMode: mode }),
}));
