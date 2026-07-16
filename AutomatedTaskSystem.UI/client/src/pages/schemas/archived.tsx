import { useEffect, useMemo, useState } from "react";
import Head from "next/head";
import Link from "next/link";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import Loader from "../../components/loader";
import API from "../../lib/API";
import SchemaIcon from "../../assets/Icons/Schema";

const ArchivedSchemas = () => {
  const [isLoading, setIsLoading] = useState(true);
  const [items, setItems] = useState<ISchema[]>([]);
  const [busyId, setBusyId] = useState<number | null>(null);

  useEffect(() => {
    API.SCHEMAS.GET_ARCHIVED().then((res: any) => {
      const arr: ISchema[] = Array.isArray(res)
        ? res
        : res && Array.isArray(res.data)
        ? (res.data as ISchema[])
        : [];
      setItems(arr);
      setIsLoading(false);
    });
  }, []);

  const handleUnarchive = async (id: number) => {
    setBusyId(id);
    const res = await API.SCHEMAS.UNARCHIVE(id);
    if (res && !res.error) {
      setItems((prev) => prev.filter((x) => x.id !== id));
    }
    setBusyId(null);
  };

  const columns: GridColDef[] = useMemo(
    () => [
      { field: "col0", headerName: "ID", width: 100 },
      { field: "col3", headerName: "Type", width: 200 },
      { field: "col1", headerName: "Name", width: 300 },
      { field: "col2", headerName: "Description", width: 300 },
      {
        field: "col4",
        headerName: "Actions",
        width: 180,
        renderCell: (c) => (
          <button
            className="px-3 py-1 rounded bg-green-600 text-white disabled:opacity-50"
            onClick={() => handleUnarchive(c.id as number)}
            disabled={busyId === (c.id as number)}
            title="Unarchive this schema"
          >
            {busyId === (c.id as number) ? "Unarchiving..." : "Unarchive"}
          </button>
        ),
        filterable: false,
        disableColumnMenu: true,
        sortable: false,
      },
    ],
    [busyId]
  );

  if (isLoading)
    return (
      <div className="flex items-center justify-center mx-auto h-full">
        <Head>
          <title>TMS - Loading</title>
        </Head>
        <Loader />
      </div>
    );

  return (
    <>
      <Head>
        <title>TMS - Archived Schemas</title>
      </Head>
      <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
        <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
          <div className="flex gap-2 items-center">
            <div className="w-6">
              <SchemaIcon />
            </div>
            <h1 className="font-bold text-2xl ">Archived Schemas</h1>
          </div>
          <div className="flex gap-2">
            <Link href={{ pathname: "/schemas" }}>
              <button className="px-4 py-1 rounded bg-blue-600 text-white">
                Back to Active
              </button>
            </Link>
          </div>
        </div>
        <div className="pb-4 mt-4">
          <DataGrid
            className="bg-white relative h-full"
            rows={items.map((p) => ({
              id: p.id,
              col0: p.id,
              col1: p.name,
              col2: p.description,
              col3: p.type ? p.type.name : "None",
            }))}
            columns={columns}
            initialState={{
              sorting: {
                sortModel: [{ field: "col3", sort: "asc" }],
              },
            }}
          />
        </div>
      </div>
    </>
  );
};

export default ArchivedSchemas;

