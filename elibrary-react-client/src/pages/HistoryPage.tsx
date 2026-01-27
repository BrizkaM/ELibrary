import { useBorrowBookRecords } from "../hooks";
import { Card, CardContent, CardHeader } from "../components";
import {
  History,
  AlertCircle,
  ArrowDownLeft,
  ArrowUpRight,
} from "lucide-react";
import type { BorrowBookRecord } from "../types";

export function HistoryPage() {
  const { data: records, isLoading, error } = useBorrowBookRecords();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="flex items-center gap-2 text-red-600">
          <AlertCircle className="h-5 w-5" />
          <span>Chyba při načítání historie: {error.message}</span>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Historie půjček</h1>
        <p className="mt-2 text-gray-600">
          Celkem {records?.length || 0} záznamů
        </p>
      </div>

      {records && records.length > 0 ? (
        <Card>
          <CardHeader>
            <h2 className="font-semibold text-gray-900">Záznamy</h2>
          </CardHeader>
          <CardContent className="p-0">
            <div className="divide-y divide-gray-200">
              {records.map((record: BorrowBookRecord) => (
                <div
                  key={record.id}
                  className="flex items-center justify-between px-6 py-4 hover:bg-gray-50"
                >
                  <div className="flex items-center gap-4">
                    {record.action === "Borrowed" ? (
                      <div className="p-2 bg-orange-100 rounded-full">
                        <ArrowUpRight className="h-5 w-5 text-orange-600" />
                      </div>
                    ) : (
                      <div className="p-2 bg-green-100 rounded-full">
                        <ArrowDownLeft className="h-5 w-5 text-green-600" />
                      </div>
                    )}
                    <div>
                      <p className="font-medium text-gray-900">
                        {record.customerName}
                      </p>
                      <p className="text-sm text-gray-600">
                        {record.action === "Borrowed"
                          ? "Půjčil/a si knihu"
                          : "Vrátil/a knihu"}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-gray-600">
                      {new Date(record.date).toLocaleDateString("cs-CZ")}
                    </p>
                    <p className="text-xs text-gray-400">
                      {new Date(record.date).toLocaleTimeString("cs-CZ")}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardContent className="py-12 text-center">
            <History className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-600">Žádné záznamy o půjčkách</p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
