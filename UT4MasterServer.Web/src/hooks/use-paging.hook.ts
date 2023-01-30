import { shallowRef } from "vue";

export function usePaging(pageSize = 10) {

    const pageStart = shallowRef(0);
    const pageEnd = shallowRef(pageSize);

    function handlePagingUpdate(start: number, end: number) {
        pageStart.value = start;
        pageEnd.value = end;
    }

    return {
        pageSize,
        pageStart,
        pageEnd,
        handlePagingUpdate
    };
}