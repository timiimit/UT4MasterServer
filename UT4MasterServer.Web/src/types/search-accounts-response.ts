import { IAccount } from './account';

export interface ISearchAccountsResponse<T extends IAccount> {
	accounts: T[];
	count: number;
}
