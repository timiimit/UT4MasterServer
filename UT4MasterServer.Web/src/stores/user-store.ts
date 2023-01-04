import { ref } from "vue";

const _username = ref<string | null>(localStorage.getItem('ut4uu_username'));
const _authToken = ref<string | null>(localStorage.getItem('ut4uu_authorizationToken'));
const _authCode = ref<string | null>(localStorage.getItem('ut4uu_authorizationCode'));

export const UserStore = {
    get isAuthenticated() {
        return !!_authCode.value && !!_authToken.value;
    },
    get saveUsername() {
        return localStorage.getItem('ut4uu_save_username') === 'true';
    },
    set saveUsername(save: boolean) {
        localStorage.setItem('ut4uu_save_username', save.toString());
    },
    get username() {
        return _username.value;
    },
    set username(user: string | null) {
        _username.value = user;
        if (user && this.saveUsername) {
            localStorage.setItem('ut4uu_username', user);
        } else {
            localStorage.removeItem('ut4uu_username');
        }
    },
    get authCode() {
        return _authCode.value;
    },
    set authCode(code: string | null) {
        _authCode.value = code;
        if (code) {
            localStorage.setItem('ut4uu_authorizationCode', code);
        } else {
            localStorage.removeItem('ut4uu_authorizationCode');
        }
    },
    get authToken() {
        return _authToken.value;
    },
    set authToken(code: string | null) {
        _authToken.value = code;
        if (code) {
            localStorage.setItem('ut4uu_authorizationToken', code);
        } else {
            localStorage.removeItem('ut4uu_authorizationToken');
        }
    }
};