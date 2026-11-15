import type { ReactNode } from 'react';
import './globals.css';

export const metadata = {
  title: 'mcp.ntsa.uk — Priority skills',
  description: 'Priority agent skill catalog MCP (shared public chrome with bob.ntsa.uk)',
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
