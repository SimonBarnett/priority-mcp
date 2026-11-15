export type PriorityAgentDownload = {
  tag: string;
  versionLabel: string;
  exeUrl: string;
  sha256Url: string | null;
};

export function pickPriorityAgentDownload(releases: unknown): PriorityAgentDownload | null;

export function resolvePriorityAgentDownload(opts?: {
  fetchImpl?: typeof fetch;
  apiUrl?: string;
}): Promise<PriorityAgentDownload | null>;
