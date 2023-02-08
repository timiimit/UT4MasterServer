export interface IPagedResponse<Type> {
  count: number;
  data: Type[];
}
