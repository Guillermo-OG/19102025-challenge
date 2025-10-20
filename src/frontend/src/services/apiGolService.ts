import { BoardState, Cell, createBoard } from "../domain/board";
import { Rule, conway, parseBsRule } from "../domain/rules";

export interface GolService {
  getBoard(): BoardState;
  setCell(x: number, y: number, alive: boolean): BoardState;
  toggleCell(x: number, y: number): BoardState;
  clear(): BoardState;
  randomize(density?: number): BoardState;

  getRule(): Rule;
  setRule(notationOrRule: string | Rule): Rule;

  next(persist?: boolean): Promise<BoardState>;
  advance(steps: number, persist?: boolean): Promise<BoardState>;
}

type CellDto = { x: number; y: number };
type CreateBoardRequest = {
  width: number;
  height: number;
  liveCells: CellDto[];
  rule?: string | null;
};
type UpdateBoardRequest = {
  liveCells: CellDto[];
  rule?: string | null;
  generation?: number | null;
};
type NextRequest = { persist: boolean; rule?: string | null };
type AdvanceRequest = { steps: number; persist: boolean; rule?: string | null };
type BoardResponse = {
  id: string;
  width: number;
  height: number;
  generation: number;
  rule: string;
  liveCells: CellDto[];
  liveCount: number;
};

function toLiveCells(board: BoardState): CellDto[] {
  const live: CellDto[] = [];
  for (let y = 0; y < board.height; y++) {
    const row = y * board.width;
    for (let x = 0; x < board.width; x++) {
      if (board.cells[row + x] === 1) live.push({ x, y });
    }
  }
  return live;
}

function fromDto(resp: BoardResponse): BoardState {
  const b = createBoard(
    resp.width,
    resp.height,
    resp.liveCells,
    resp.generation
  );
  return b;
}

async function http<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: {
      "Content-Type": "application/json",
      ...(options?.headers ?? {}),
    },
    ...options,
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`HTTP ${res.status}: ${text || res.statusText}`);
  }
  return (await res.json()) as T;
}

const API_BASE = "/api/boards";

export class ApiGolService implements GolService {
  private board: BoardState;
  private rule: Rule;
  private boardId: string | null = null;
  private dirty = false;
  private pendingRule: Rule | null = null;

  constructor(width = 50, height = 30, rule: Rule = conway()) {
    this.board = createBoard(width, height);
    this.rule = rule;
  }

  async init(initialLiveCells: Cell[] = []): Promise<BoardState> {
    if (this.boardId) return this.board;
    const req: CreateBoardRequest = {
      width: this.board.width,
      height: this.board.height,
      liveCells: initialLiveCells.map((c) => ({ x: c.x, y: c.y })),
      rule: this.rule.notation,
    };
    const resp = await http<BoardResponse>(API_BASE, {
      method: "POST",
      body: JSON.stringify(req),
    });
    this.boardId = resp.id;
    this.rule = parseBsRule(resp.rule);
    this.board = fromDto(resp);
    this.dirty = false;
    this.pendingRule = null;
    return this.board;
  }

  getBoard(): BoardState {
    return this.board;
  }

  getRule(): Rule {
    return this.rule;
  }

  setRule(notationOrRule: string | Rule): Rule {
    const r =
      typeof notationOrRule === "string"
        ? parseBsRule(notationOrRule)
        : notationOrRule;
    this.rule = r;
    this.pendingRule = r;
    return this.rule;
  }

  setCell(x: number, y: number, alive: boolean): BoardState {
    const idx = y * this.board.width + x;
    if (x < 0 || y < 0 || x >= this.board.width || y >= this.board.height)
      return this.board;
    if ((this.board.cells[idx] === 1) === alive) return this.board;
    const copy = new Uint8Array(this.board.cells);
    copy[idx] = alive ? 1 : 0;
    this.board = { ...this.board, cells: copy };
    this.dirty = true;
    return this.board;
  }

  toggleCell(x: number, y: number): BoardState {
    const idx = y * this.board.width + x;
    if (x < 0 || y < 0 || x >= this.board.width || y >= this.board.height)
      return this.board;
    const copy = new Uint8Array(this.board.cells);
    copy[idx] = copy[idx] ^ 1;
    this.board = { ...this.board, cells: copy };
    this.dirty = true;
    return this.board;
  }

  clear(): BoardState {
    this.board = createBoard(this.board.width, this.board.height);
    this.dirty = true;
    return this.board;
  }

  randomize(density = 0.25): BoardState {
    const cells = new Uint8Array(this.board.width * this.board.height);
    for (let i = 0; i < cells.length; i++) {
      cells[i] = Math.random() < density ? 1 : 0;
    }
    this.board = { ...this.board, cells, generation: 0 };
    this.dirty = true;
    return this.board;
  }

  private async ensureCreated(): Promise<void> {
    if (!this.boardId) await this.init();
  }

  private async syncIfDirty(): Promise<void> {
    await this.ensureCreated();
    if (!this.boardId) throw new Error("Board not created");
    if (!this.dirty && !this.pendingRule) return;

    const req: UpdateBoardRequest = {
      liveCells: toLiveCells(this.board),
      rule: this.pendingRule ? this.pendingRule.notation : undefined,
      generation: this.board.generation ?? 0,
    };
    const resp = await http<BoardResponse>(`${API_BASE}/${this.boardId}`, {
      method: "PUT",
      body: JSON.stringify(req),
    });
    this.rule = parseBsRule(resp.rule);
    this.board = fromDto(resp);
    this.dirty = false;
    this.pendingRule = null;
  }

  async next(persist = true): Promise<BoardState> {
    await this.syncIfDirty();
    if (!this.boardId) throw new Error("Board not created");
    const req: NextRequest = { persist, rule: undefined };
    const resp = await http<BoardResponse>(`${API_BASE}/${this.boardId}/next`, {
      method: "POST",
      body: JSON.stringify(req),
    });
    this.rule = parseBsRule(resp.rule);
    const next = fromDto(resp);
    if (persist) this.board = next;
    return persist ? this.board : next;
  }

  async advance(steps: number, persist = true): Promise<BoardState> {
    await this.syncIfDirty();
    if (!this.boardId) throw new Error("Board not created");
    const req: AdvanceRequest = { steps, persist, rule: undefined };
    const resp = await http<BoardResponse>(
      `${API_BASE}/${this.boardId}/advance`,
      {
        method: "POST",
        body: JSON.stringify(req),
      }
    );
    this.rule = parseBsRule(resp.rule);
    const result = fromDto(resp);
    if (persist) this.board = result;
    return persist ? this.board : result;
  }
}
