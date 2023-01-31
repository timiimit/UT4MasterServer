export function validateEmail(input?: string) {
  if (!input) {
    return false;
  }
  const regexp = new RegExp(/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/);
  return regexp.test(input);
}

export function validatePassword(password?: string) {
  return password && password.length >= 7;
}
