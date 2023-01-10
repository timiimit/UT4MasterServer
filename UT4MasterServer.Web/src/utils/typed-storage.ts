export const TypedStorage = {
    getItem<T>(key: string): T | null {
        const itemStr = localStorage.getItem(key);
        return itemStr ? JSON.parse(itemStr) as T : null;
    },
    setItem<T>(key: string, value: T | null) {
        localStorage.setItem(key, JSON.stringify(value));
    }
};