import React, { useEffect, useRef } from "react";
import { BoardState } from "../domain/board";

type Props = {
  board: BoardState;
  cellSize?: number;
  aliveColor?: string;
  deadColor?: string;
  grid?: boolean;
  onToggle?: (x: number, y: number) => void;
  onPaint?: (x: number, y: number, alive: boolean) => void;
};

export default function CanvasBoard({
  board,
  cellSize = 16,
  aliveColor = "#61dafb",
  deadColor = "#0f1218",
  grid = true,
  onToggle,
  onPaint,
}: Props) {
  const ref = useRef<HTMLCanvasElement | null>(null);
  const isDown = useRef(false);
  const paintValue = useRef<boolean | null>(null);

  useEffect(() => {
    const canvas = ref.current!;
    const ctx = canvas.getContext("2d")!;
    canvas.width = board.width * cellSize;
    canvas.height = board.height * cellSize;

    ctx.fillStyle = deadColor;
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.fillStyle = aliveColor;
    for (let i = 0; i < board.cells.length; i++) {
      if (board.cells[i] === 1) {
        const x = (i % board.width) * cellSize;
        const y = Math.floor(i / board.width) * cellSize;
        ctx.fillRect(x, y, cellSize, cellSize);
      }
    }

    if (grid && cellSize >= 8) {
      ctx.strokeStyle = "rgba(255,255,255,0.08)";
      ctx.lineWidth = 1;
      for (let x = 0; x <= board.width; x++) {
        const xx = x * cellSize + 0.5;
        ctx.beginPath();
        ctx.moveTo(xx, 0);
        ctx.lineTo(xx, canvas.height);
        ctx.stroke();
      }
      for (let y = 0; y <= board.height; y++) {
        const yy = y * cellSize + 0.5;
        ctx.beginPath();
        ctx.moveTo(0, yy);
        ctx.lineTo(canvas.width, yy);
        ctx.stroke();
      }
    }
  }, [board, cellSize, aliveColor, deadColor, grid]);

  const getCellFromEvent = (e: React.MouseEvent<HTMLCanvasElement>) => {
    const rect = (e.target as HTMLCanvasElement).getBoundingClientRect();
    const x = Math.floor((e.clientX - rect.left) / cellSize);
    const y = Math.floor((e.clientY - rect.top) / cellSize);
    return { x, y };
  };

  return (
    <canvas
      ref={ref}
      style={{
        border: "1px solid #2a2f3a",
        borderRadius: 6,
        background: deadColor,
        cursor: "pointer",
      }}
      onMouseDown={(e) => {
        isDown.current = true;
        const { x, y } = getCellFromEvent(e);
        if (onPaint) {
          paintValue.current = e.button === 2 ? false : true;
          onPaint(x, y, paintValue.current);
        } else {
          onToggle?.(x, y);
        }
      }}
      onMouseMove={(e) => {
        if (!isDown.current) return;
        const { x, y } = getCellFromEvent(e);
        if (onPaint && paintValue.current !== null)
          onPaint(x, y, paintValue.current);
      }}
      onMouseUp={() => {
        isDown.current = false;
        paintValue.current = null;
      }}
      onMouseLeave={() => {
        isDown.current = false;
        paintValue.current = null;
      }}
      onContextMenu={(e) => e.preventDefault()}
    />
  );
}
