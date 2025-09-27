import { PaginationInfo } from "@/types/kerko";

interface PaginationProps {
  pagination: PaginationInfo;
  onPageChange: (page: number) => void;
}

export function Pagination({ pagination, onPageChange }: PaginationProps) {
  const { currentPage, totalPages, hasPrevious, hasNext } = pagination;

  if (totalPages <= 1) {
    return null;
  }

  const generatePageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisiblePages = 5;

    if (totalPages <= maxVisiblePages) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);

      if (currentPage > 3) {
        pages.push("...");
      }

      const start = Math.max(2, currentPage - 1);
      const end = Math.min(totalPages - 1, currentPage + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (currentPage < totalPages - 2) {
        pages.push("...");
      }

      if (totalPages > 1) {
        pages.push(totalPages);
      }
    }

    return pages;
  };

  return (
    <div className="flex items-center justify-center gap-2 mt-6 mb-4">
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={!hasPrevious}
        className={`px-3 py-2 text-sm rounded-md transition-colors ${
          hasPrevious
            ? "bg-[#2a1a1a] text-white hover:bg-[#3a2a2a] border border-[#444]"
            : "bg-[#1a1a1a] text-[#666] cursor-not-allowed border border-[#333]"
        }`}
      >
        ← Para
      </button>

      <div className="flex items-center gap-1">
        {generatePageNumbers().map((page, index) => (
          <button
            key={index}
            onClick={() => typeof page === "number" && onPageChange(page)}
            disabled={typeof page === "string"}
            className={`px-3 py-2 text-sm rounded-md transition-colors min-w-[40px] ${
              page === currentPage
                ? "bg-[#444] text-[#ffffff] border border-[#666] font-semibold"
                : typeof page === "string"
                ? "bg-transparent text-[#666] cursor-default"
                : "bg-[#2a1a1a] text-white hover:bg-[#3a2a2a] border border-[#444]"
            }`}
          >
            {page}
          </button>
        ))}
      </div>

      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={!hasNext}
        className={`px-3 py-2 text-sm rounded-md transition-colors ${
          hasNext
            ? "bg-[#2a1a1a] text-white hover:bg-[#3a2a2a] border border-[#444]"
            : "bg-[#1a1a1a] text-[#666] cursor-not-allowed border border-[#333]"
        }`}
      >
        Pas →
      </button>
    </div>
  );
}
