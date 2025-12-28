import { shallowRef } from 'vue';

export function usePaging(pageSize = 10) {
	const pageStart = shallowRef(0);
	const pageEnd = shallowRef(pageSize);
	const currentPage = shallowRef(0);

	function handlePagingUpdate(start: number, end: number, page: number) {
		pageStart.value = start;
		pageEnd.value = end;
		currentPage.value = page;
	}

	return {
		pageSize,
		pageStart,
		pageEnd,
		currentPage,
		handlePagingUpdate
	};
}
