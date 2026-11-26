import type { Pager } from "../hooks/usePagination";

export default function Pagination({ pager }: { pager: Pager }) {
  if (pager.total === 0) return null;
  const btn =
    "px-3 py-2 rounded border text-sm bg-white hover:bg-gray-50 disabled:opacity-40 disabled:hover:bg-white";
  return (
    <div className="flex flex-col sm:flex-row items-center justify-between gap-2 mt-4 text-sm text-gray-500">
      <span>
        Showing {pager.start + 1}–{pager.end} of {pager.total}
      </span>
      <div className="flex items-center gap-2">
        <button className={btn} disabled={pager.page <= 1}
          onClick={() => pager.setPage(pager.page - 1)}>Previous</button>
        <span>Page {pager.page} of {pager.totalPages}</span>
        <button className={btn} disabled={pager.page >= pager.totalPages}
          onClick={() => pager.setPage(pager.page + 1)}>Next</button>
      </div>
    </div>
  );
}