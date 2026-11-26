import { useState } from "react";

export function usePagination<T>(items: T[], pageSize = 10) {
  const [page, setPage] = useState(1);
  const totalPages = Math.max(1, Math.ceil(items.length / pageSize));
  const current = Math.min(page, totalPages);
  const start = (current - 1) * pageSize;
  return {
    page: current,
    setPage,
    totalPages,
    total: items.length,
    start,
    end: Math.min(start + pageSize, items.length),
    pageItems: items.slice(start, start + pageSize),
  };
}

export type Pager = ReturnType<typeof usePagination>;