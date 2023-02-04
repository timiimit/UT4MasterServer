import { TypedStorage } from '@/utils/typed-storage';
import { ref } from 'vue';
import { IAccountExtended } from '@/types/account';
import AccountService from '@/services/account.service';
import { SessionStore } from './session-store';
import { Role } from '@/enums/role';

const _account = ref<IAccountExtended | null>(null);
const _accountService = new AccountService();
const _adminRoles = [Role.Admin, Role.Moderator];

export const AccountStore = {
  get account() {
    return _account.value;
  },
  set account(account: IAccountExtended | null) {
    _account.value = account;
    TypedStorage.setItem<IAccountExtended>(
      `account-${SessionStore.session?.session_id}`,
      account
    );
  },
  get isAdmin() {
    return _account.value?.roles?.some((r) => _adminRoles.includes(r));
  },
  async fetchUserAccount() {
    try {
      _account.value = SessionStore.session?.account_id
        ? await _accountService.getAccount(SessionStore.session.account_id)
        : null;
    } catch (err: unknown) {
      console.error('Error fetching user account:', err);
    }
  }
};
