import { cn } from "@/lib/utils";
import { LucideIcon } from "lucide-react";

export const cardStyles = {
  root: "overflow-hidden border-2 border-border-semantic-secondary bg-surface-primary",
  header:
    "bg-gradient-to-br from-surface-secondary to-surface-primary py-4 px-5 border-b-2 border-border-semantic-secondary relative",
  title: "text-xl font-bold tracking-tight text-text-primary uppercase",
  infoList: "mt-1.5 space-y-2",
  infoItem: "text-text-secondary flex items-start gap-2 text-sm",
  infoIcon: "h-3.5 w-3.5 text-text-secondary mt-0.5",
  infoLabel: "text-text-tertiary font-normal min-w-[120px]",
  infoValue: "text-text-primary uppercase text-right flex-1",
  content: "p-0",
  section: "px-5 pb-3",
  sectionTitle:
    "text-base font-bold text-text-primary mb-2 flex items-center gap-2",
  sectionIcon: "h-4 w-4 text-text-secondary",
  detailsContainer:
    "bg-surface-secondary rounded-lg p-2 border-2 border-border-semantic-secondary",
  detailsGrid: "grid grid-cols-1 gap-2",
  detailsRow: "flex items-start justify-between gap-4",
  detailsLabel: "text-text-secondary font-normal min-w-[120px]",
  detailsValue: "text-text-primary font-bold uppercase text-right flex-1",
  comment: "text-sm text-text-primary mt-2 font-normal italic",
};

export const InfoItem = ({
  icon: Icon,
  label,
  value,
  className,
}: {
  icon: LucideIcon;
  label: string;
  value: string | React.ReactNode;
  className?: string;
}) => (
  <div className={cn(cardStyles.infoItem, className)}>
    <Icon className={cardStyles.infoIcon} />
    <span className={cardStyles.infoLabel}>{label}</span>
    {typeof value === "string" ? (
      <span className={cardStyles.infoValue}>{value}</span>
    ) : (
      value
    )}
  </div>
);

export const DetailRow = ({
  label,
  value,
  className,
  icon: Icon,
}: {
  label: string;
  value: React.ReactNode;
  className?: string;
  icon?: LucideIcon;
}) => (
  <div className={cn(cardStyles.detailsRow, className)}>
    <div className="flex items-center gap-1">
      {Icon && <Icon className="w-3 h-3 text-text-tertiary" />}
      <span className={cardStyles.detailsLabel}>{label}</span>
    </div>
    <span className={cardStyles.detailsValue}>{value}</span>
  </div>
);
