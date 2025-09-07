// Custom URL polyfill for browser compatibility
// This provides the Node.js URL API in the browser

export class URL {
  public href: string
  public protocol: string
  public hostname: string
  public port: string
  public pathname: string
  public search: string
  public hash: string
  public origin: string
  public host: string

  constructor(url: string, base?: string) {
    const nativeUrl = new globalThis.URL(url, base)
    this.href = nativeUrl.href
    this.protocol = nativeUrl.protocol
    this.hostname = nativeUrl.hostname
    this.port = nativeUrl.port
    this.pathname = nativeUrl.pathname
    this.search = nativeUrl.search
    this.hash = nativeUrl.hash
    this.origin = nativeUrl.origin
    this.host = nativeUrl.host
  }

  toString(): string {
    return this.href
  }
}

export class URLSearchParams {
  private params: globalThis.URLSearchParams

  constructor(
    init?:
      | string
      | string[][]
      | Record<string, string>
      | globalThis.URLSearchParams
  ) {
    this.params = new globalThis.URLSearchParams(init as any)
  }

  append(name: string, value: string): void {
    this.params.append(name, value)
  }

  delete(name: string): void {
    this.params.delete(name)
  }

  get(name: string): string | null {
    return this.params.get(name)
  }

  getAll(name: string): string[] {
    return this.params.getAll(name)
  }

  has(name: string): boolean {
    return this.params.has(name)
  }

  set(name: string, value: string): void {
    this.params.set(name, value)
  }

  sort(): void {
    this.params.sort()
  }

  toString(): string {
    return this.params.toString()
  }

  *[Symbol.iterator](): Iterator<[string, string]> {
    for (const [key, value] of this.params) {
      yield [key, value]
    }
  }
}
