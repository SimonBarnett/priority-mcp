import type { ReactNode } from 'react';
import './globals.css';

export const metadata = {
  title: 'mcp.ntsa.uk — Priority MCP',
  description: 'Priority MCP (shared public chrome with bob.ntsa.uk)',
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
