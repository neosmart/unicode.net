declare interface Glyph {
    id: number,
}

declare interface Font {
    layout(symbol: string): {
        glyphs: Glyph[],
    }
}
