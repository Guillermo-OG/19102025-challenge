import { createBoard, signature } from "../domain/board";
import { conway } from "../domain/rules";
import { advance, nextState } from "../domain/engine";

const rule = conway();

test("Still life: 2x2 block remains stable", () => {
  const state = createBoard(4, 4, [
    { x: 1, y: 1 },
    { x: 2, y: 1 },
    { x: 1, y: 2 },
    { x: 2, y: 2 },
  ]);
  const next = nextState(state, rule);
  expect(next.generation).toBe(state.generation + 1);
  expect(signature(next)).toBe(signature(state));
});

test("Oscillator: blinker period 2", () => {
  const state = createBoard(5, 5, [
    { x: 1, y: 2 },
    { x: 2, y: 2 },
    { x: 3, y: 2 },
  ]);
  const next1 = nextState(state, rule);
  const next2 = nextState(next1, rule);
  expect(signature(next2)).toBe(signature(state));
});

test("Empty board stays empty, and advance doesn't blow up", () => {
  const state = createBoard(3, 3, []);
  const result = advance(state, rule, 10);
  expect(signature(result)).toBe(signature(state));
});
