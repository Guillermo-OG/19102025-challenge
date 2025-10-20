export interface BoardState {
  width: number;
  height: number;
  generation: number;
  cells: Uint8Array;
}

export interface Cell {
  x: number;
  y: number;
}

export function createBoard(
  width: number,
  height: number,
  liveCells: Cell[] = [],
  generation = 0
): BoardState {
  if (width <= 0 || height <= 0)
    throw new Error("Board dimensions must be positive");
  const cells = new Uint8Array(width * height);
  for (const { x, y } of liveCells) {
    if (x < 0 || y < 0 || x >= width || y >= height)
      throw new Error(`Cell out of bounds: (${x},${y})`);
    cells[y * width + x] = 1;
  }
  return { width, height, generation, cells };
}

export function indexOf(board: BoardState, x: number, y: number): number {
  return y * board.width + x;
}

export function isAlive(board: BoardState, x: number, y: number): boolean {
  if (x < 0 || y < 0 || x >= board.width || y >= board.height) return false;
  return board.cells[indexOf(board, x, y)] === 1;
}

export function setAlive(
  board: BoardState,
  x: number,
  y: number,
  alive: boolean
): BoardState {
  if (x < 0 || y < 0 || x >= board.width || y >= board.height) return board;
  const idx = indexOf(board, x, y);
  if ((board.cells[idx] === 1) === alive) return board;
  const copy = new Uint8Array(board.cells);
  copy[idx] = alive ? 1 : 0;
  return { ...board, cells: copy };
}

export function toggle(board: BoardState, x: number, y: number): BoardState {
  if (x < 0 || y < 0 || x >= board.width || y >= board.height) return board;
  const idx = indexOf(board, x, y);
  const copy = new Uint8Array(board.cells);
  copy[idx] = copy[idx] ^ 1;
  return { ...board, cells: copy };
}

export function isEmpty(board: BoardState): boolean {
  for (let i = 0; i < board.cells.length; i++)
    if (board.cells[i] === 1) return false;
  return true;
}

export function signature(board: BoardState): string {
  const bitCount = board.cells.length;
  const byteCount = (bitCount + 7) >> 3;
  const bytes = new Uint8Array(byteCount);

  for (let i = 0; i < bitCount; i++) {
    if (board.cells[i] === 1) {
      const bIndex = i >> 3;
      const bOffset = i & 7;
      bytes[bIndex] |= 1 << bOffset;
    }
  }
  return `${board.width}x${board.height}:${base64FromBytes(bytes)}`;
}

function base64FromBytes(bytes: Uint8Array): string {
  if (typeof window !== "undefined" && typeof window.btoa === "function") {
    let binary = "";
    const chunk = 0x8000;
    for (let i = 0; i < bytes.length; i += chunk) {
      const sub = bytes.subarray(i, i + chunk);
      binary += String.fromCharCode(...Array.from(sub));
    }
    return window.btoa(binary);
  }

  const g = globalThis as unknown as {
    Buffer?: {
      from: (arr: Uint8Array) => { toString: (enc: "base64") => string };
    };
  };
  if (g.Buffer) return g.Buffer.from(bytes).toString("base64");
  throw new Error("No base64 support");
}
