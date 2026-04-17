export class InactiveAccountError extends Error {
  constructor() {
    super('Your account has been deactivated.')
    this.name = 'InactiveAccountError'
  }
}
