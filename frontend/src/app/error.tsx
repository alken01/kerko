"use client";

import { useEffect } from "react";

// Static text instead of useTranslation: this boundary must render even if a
// provider itself is what crashed.
export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="min-h-screen bg-surface-primary flex items-center justify-center px-4">
      <div className="text-center space-y-4">
        <h1 className="text-2xl font-bold text-text-primary">Diçka shkoi keq</h1>
        <p className="text-sm text-text-secondary">
          Ndodhi një gabim i papritur. / Something went wrong.
        </p>
        <button
          onClick={reset}
          className="px-4 py-2 rounded-xl bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary hover:bg-surface-tertiary transition-colors"
        >
          Provo përsëri / Try again
        </button>
      </div>
    </div>
  );
}
