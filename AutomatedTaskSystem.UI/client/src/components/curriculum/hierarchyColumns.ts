import { GridColDef } from "@mui/x-data-grid";

export const hierarchyActionsColumn: GridColDef = {
    field: "colActions",
    headerName: "Actions",
    width: 90,
    sortable: false,
    filterable: false,
    disableColumnMenu: true,
};
