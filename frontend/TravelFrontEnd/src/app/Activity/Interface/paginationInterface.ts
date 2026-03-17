import { CardInfoModel } from "./cardInterface";

export interface paginationInterface {
  data: CardInfoModel[],
  pageNumber: number,
  pageSize: number,
  totalPages: number,
  totalRecords: number
}
