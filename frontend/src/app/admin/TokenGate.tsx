"use client";

import { useState, FormEvent } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { cardStyles } from "@/components/ui/card-styles";
import { fetchStats, AdminUnauthorizedError, TOKEN_KEY } from "./api";

interface TokenGateProps {
  onAuthenticated: () => void;
}

export function TokenGate({ onAuthenticated }: TokenGateProps) {
  const [token, setToken] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!token.trim()) return;

    setLoading(true);
    setError(null);

    try {
      await fetchStats("1h", token.trim());
      localStorage.setItem(TOKEN_KEY, token.trim());
      onAuthenticated();
    } catch (err) {
      if (err instanceof AdminUnauthorizedError) {
        setError("Invalid token. Please try again.");
      } else {
        setError("Connection error. Check network and try again.");
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-h-screen bg-surface-primary flex items-center justify-center px-4">
      <div className="w-full max-w-sm">
        <Card className={cardStyles.root}>
          <CardHeader className="text-center">
            <CardTitle className="text-2xl font-bold tracking-tight text-text-primary">Admin</CardTitle>
            <CardDescription className="text-text-tertiary">Enter your admin token to continue.</CardDescription>
          </CardHeader>
          <CardContent className="px-5 pb-5">
            <form onSubmit={handleSubmit} className="space-y-4">
              <Input
                type="password"
                value={token}
                onChange={(e) => setToken(e.target.value)}
                placeholder="Admin token"
                autoComplete="current-password"
                className="bg-surface-secondary border-2 border-border-semantic-secondary placeholder:text-text-tertiary focus-visible:ring-border-semantic-interactive focus-visible:ring-offset-0 h-12 touch-manipulation"
                disabled={loading}
              />

              {error && (
                <Alert variant="destructive">
                  <AlertDescription>{error}</AlertDescription>
                </Alert>
              )}

              <Button
                type="submit"
                variant="outline"
                disabled={loading || !token.trim()}
                className="w-full bg-surface-secondary border-2 border-border-semantic-secondary hover:bg-surface-tertiary h-12 touch-manipulation"
              >
                {loading ? "Verifying..." : "Sign in"}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
