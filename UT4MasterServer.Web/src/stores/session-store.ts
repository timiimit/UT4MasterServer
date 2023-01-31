import { TypedStorage } from '@/utils/typed-storage';
import { ISession } from '@/types/session';
import { ref } from 'vue';

const _session = ref<ISession | null>(
  TypedStorage.getItem<ISession>('session')
);

export const SessionStore = {
  get isAuthenticated() {
    return !!_session.value?.access_token;
  },
  get saveUsername() {
    return TypedStorage.getItem<boolean>('save_username') === true;
  },
  set saveUsername(save: boolean) {
    TypedStorage.setItem<boolean>('save_username', save);
  },
  get username() {
    return _session.value?.displayName;
  },
  get token() {
    return _session.value?.access_token;
  },
  get session() {
    return _session.value;
  },
  set session(session: ISession | null) {
    _session.value = session;
    TypedStorage.setItem<ISession>('session', session);
  },
};
