import { Directive, DirectiveBinding } from 'vue';

export const valid: Directive = {
  mounted: (
    element: HTMLInputElement,
    binding: DirectiveBinding<boolean>
  ): void => {
    element.setCustomValidity(binding.value ? '' : 'invalid');
  },
  updated: (
    element: HTMLInputElement,
    binding: DirectiveBinding<boolean>
  ): void => {
    element.setCustomValidity(binding.value ? '' : 'invalid');
  }
};
