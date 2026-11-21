import type { ReactNode } from 'react';
import './globals.css';

export const metadata = {
  title: 'mcp.ntsa.uk — Priority MCP',
  description: 'Priority MCP public skill catalog (plan.ntsa.uk document formatting).',
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
