import { BoardState, isAlive } from "./board";
import { Rule, nextCell } from "./rules";

export function nextState(board: BoardState, rule: Rule): BoardState {
  const { width, height } = board;
  const next = new Uint8Array(width * height);

  for (let y = 0; y < height; y++) {
    const row = y * width;
    for (let x = 0; x < width; x++) {
      let neighbors = 0;
      for (let dy = -1; dy <= 1; dy++) {
        const ny = y + dy;
        if (ny < 0 || ny >= height) continue;
        for (let dx = -1; dx <= 1; dx++) {
          const nx = x + dx;
          if (dx === 0 && dy === 0) continue;
          if (nx < 0 || nx >= width) continue;
          neighbors += board.cells[ny * width + nx];
        }
      }
      const alive = board.cells[row + x] === 1;
      next[row + x] = nextCell(alive, neighbors, rule) ? 1 : 0;
    }
  }
  return { ...board, cells: next, generation: board.generation + 1 };
}

export function advance(
  board: BoardState,
  rule: Rule,
  steps: number
): BoardState {
  if (steps < 0) throw new Error("steps must be >= 0");
  let cur = board;
  for (let i = 0; i < steps; i++) cur = nextState(cur, rule);
  return cur;
}
