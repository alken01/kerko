import Link from "next/link";

export default function NotFound() {
  return (
    <div className="min-h-screen bg-surface-primary flex items-center justify-center px-4">
      <div className="text-center space-y-4">
        <h1 className="text-4xl font-bold text-text-primary">404</h1>
        <p className="text-sm text-text-secondary">
          Faqja nuk u gjet. / Page not found.
        </p>
        <Link
          href="/"
          className="inline-block px-4 py-2 rounded-xl bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary hover:bg-surface-tertiary transition-colors"
        >
          Kthehu te kërkimi / Back to search
        </Link>
      </div>
    </div>
  );
}
