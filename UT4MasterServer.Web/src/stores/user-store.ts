import { ref } from "vue";

const _username = ref<string | null>(localStorage.getItem('username'));
const _authToken = ref<string | null>(localStorage.getItem('authorizationToken'));

export const UserStore = {
    get isAuthenticated() {
        return !!_authToken.value;
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