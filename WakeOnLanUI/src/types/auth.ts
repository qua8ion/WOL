export interface IAuthModel {
  username: string
  password: string
}

export interface IAuthResponse {
  accessToken: string
  username: string
}

export interface TokenValidStateModel {
  valid: boolean
  accessToken: string
}
