import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { cardStyles } from "@/components/ui/card-styles";

export function SkeletonCard() {
  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
        {/* Name title */}
        <Skeleton className="h-6 w-40 mb-3" />
        {/* Three info rows */}
        <div className="space-y-2 mt-1.5">
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-3/4" />
          <Skeleton className="h-4 w-2/3" />
        </div>
      </CardHeader>
      <CardContent className={cardStyles.content}>
        <div className="px-5 py-3">
          <Skeleton className="h-5 w-36" />
        </div>
      </CardContent>
    </Card>
  );
}

export function SkeletonGrid({ count = 4 }: { count?: number }) {
  return (
    <div className="flex flex-col gap-4 max-w-4xl mx-auto w-full">
      {/* Skeleton tabs */}
      <div className="flex space-x-1 p-1 bg-surface-secondary rounded-lg border-2 border-border-semantic-secondary">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="flex-1 h-8 rounded-md" />
        ))}
      </div>
      {/* Skeleton cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start">
        {Array.from({ length: count }).map((_, i) => (
          <SkeletonCard key={i} />
        ))}
      </div>
    </div>
  );
}
