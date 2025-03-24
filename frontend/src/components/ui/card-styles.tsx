import { cn } from "@/lib/utils";

export const cardStyles = {
  root: "overflow-hidden border-2 border-[#2a1a1a] bg-[#0a0303] shadow-[0_0_15px_rgba(0,0,0,0.5)]",
  header:
    "bg-gradient-to-br from-[#120606] to-[#0a0303] py-4 px-5 border-b-2 border-[#2a1a1a] relative",
  title: "text-xl font-bold tracking-tight text-white uppercase",
  infoList: "mt-1.5 space-y-2",
  infoItem: "text-[#999] flex items-start gap-2 text-sm",
  infoIcon: "h-3.5 w-3.5 text-[#cccccc] mt-0.5",
  infoLabel: "text-[#666] font-normal min-w-[120px]",
  infoValue: "text-white uppercase text-right flex-1",
  content: "p-0",
  section: "px-5 py-3",
  sectionTitle: "text-base font-bold text-white mb-2 flex items-center gap-2",
  sectionIcon: "h-4 w-4 text-[#cccccc]",
  detailsContainer:
    "bg-[#120606] rounded-lg p-3 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2),0_1px_1px_rgba(255,255,255,0.05)] border-2 border-[#2a1a1a]",
  detailsGrid: "grid grid-cols-1 sm:grid-cols-2 gap-3",
  detailsRow: "flex items-start justify-between gap-4",
  detailsLabel: "text-[#999] font-normal min-w-[120px]",
  detailsValue: "text-white font-bold uppercase text-right flex-1",
  comment: "text-sm text-white mt-2 font-normal italic",
};

export const InfoItem = ({
  icon: Icon,
  label,
  value,
  className,
}: {
  icon: React.ElementType;
  label: string;
  value: string | React.ReactNode;
  className?: string;
}) => (
  <p className={cn(cardStyles.infoItem, className)}>
    <Icon className={cardStyles.infoIcon} />
    <span className={cardStyles.infoLabel}>{label}:</span>
    {typeof value === "string" ? (
      <span className={cardStyles.infoValue}>{value}</span>
    ) : (
      value
    )}
  </p>
);

export const DetailRow = ({
  label,
  value,
  className,
}: {
  label: string;
  value: string | React.ReactNode;
  className?: string;
}) => (
  <div className={cn(cardStyles.detailsRow, className)}>
    <span className={cardStyles.detailsLabel}>{label}</span>
    {typeof value === "string" ? (
      <span className={cardStyles.detailsValue}>{value}</span>
    ) : (
      value
    )}
  </div>
);
