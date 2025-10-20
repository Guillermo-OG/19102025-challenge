export interface Rule {
  name: string;
  notation: string;
  birth: boolean[];
  survival: boolean[];
}

export const conway = (): Rule => ({
  name: "Conway B3/S23",
  notation: "B3/S23",
  birth: toFlags([3]),
  survival: toFlags([2, 3]),
});

export function toFlags(values: number[]): boolean[] {
  const flags = Array(9).fill(false);
  for (const v of values) {
    if (v < 0 || v > 8) throw new Error("Neighbor counts must be 0..8");
    flags[v] = true;
  }
  return flags;
}

export function parseBsRule(notation: string): Rule {
  if (!notation || !notation.trim()) throw new Error("Rule notation is empty");
  const norm = notation.trim().toUpperCase().replace(/\s+/g, "");
  const parts = norm.split("/");
  if (parts.length !== 2)
    throw new Error("Invalid rule format. Expect 'B.../S...'");
  const b = parts.find((p) => p.startsWith("B"));
  const s = parts.find((p) => p.startsWith("S"));
  if (!b || !s) throw new Error("Rule must contain 'B' and 'S' sections");

  const birth: boolean[] = Array(9).fill(false);
  const survival: boolean[] = Array(9).fill(false);

  const parsePart = (part: string, prefix: string, target: boolean[]) => {
    if (!part.startsWith(prefix))
      throw new Error(`Part must start with '${prefix}'`);
    for (let i = 1; i < part.length; i++) {
      const ch = part[i];
      if (!/\d/.test(ch)) throw new Error("Non-digit in rule counts");
      const v = ch.charCodeAt(0) - 48;
      if (v < 0 || v > 8) throw new Error("Counts must be between 0 and 8");
      target[v] = true;
    }
  };

  parsePart(b, "B", birth);
  parsePart(s, "S", survival);

  return {
    name: `Rule ${norm}`,
    notation: norm,
    birth,
    survival,
  };
}

export function nextCell(
  alive: boolean,
  neighbors: number,
  rule: Rule
): boolean {
  if (neighbors < 0 || neighbors > 8) return false;
  return alive ? rule.survival[neighbors] : rule.birth[neighbors];
}
