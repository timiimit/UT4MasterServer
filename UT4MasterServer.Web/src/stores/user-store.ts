import { ref } from "vue";

const _username = ref<string | null>(localStorage.getItem('username'));
const _authToken = ref<string | null>(localStorage.getItem('authorizationToken'));
const _authCode = ref<string | null>(localStorage.getItem('authorizationCode'));

export const UserStore = {
    get isAuthenticated() {
        return !!_authCode.value && !!_authToken.value;
    },
    get saveUsername() {
        return localStorage.getItem('save_username') === 'true';
    },
    set saveUsername(save: boolean) {
        localStorage.setItem('save_username', save.toString());
    },
    get username() {
        return _username.value;
    },
    set username(user: string | null) {
        _username.value = user;
        if (user && this.saveUsername) {
            localStorage.setItem('username', user);
        } else {
            localStorage.removeItem('username');
        }
    },
    get authCode() {
        return _authCode.value;
    },
    set authCode(code: string | null) {
        _authCode.value = code;
        if (code) {
            localStorage.setItem('authorizationCode', code);
        } else {
            localStorage.removeItem('authorizationCode');
        }
    },
    get authToken() {
        return _authToken.value;
    },
    set authToken(code: string | null) {
        _authToken.value = code;
        if (code) {
            localStorage.setItem('authorizationToken', code);
        } else {
            localStorage.removeItem('authorizationToken');
        }
    }
};