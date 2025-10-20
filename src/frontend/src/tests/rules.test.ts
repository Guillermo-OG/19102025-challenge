import { conway, parseBsRule, nextCell } from "../domain/rules";

test("parse B3/S23", () => {
  const r = parseBsRule("B3/S23");
  expect(r.notation).toBe("B3/S23");

  expect(nextCell(false, 3, r)).toBe(true);

  expect(nextCell(true, 2, r)).toBe(true);
});

test("invalid rules rejected", () => {
  expect(() => parseBsRule("B9/S23")).toThrow();
  expect(() => parseBsRule("something")).toThrow();
});

test("conway defaults", () => {
  const r = conway();
  expect(r.notation).toBe("B3/S23");
});
