import React, { useEffect, useMemo, useState } from "react";
import "./App.css";
import CanvasBoard from "./components/CanvasBoard";
import ControlPanel from "./components/ControlPanel";
import { useInterval } from "./hooks/useInterval";
import { createBoard } from "./domain/board";
import { conway } from "./domain/rules";
import { ApiGolService } from "./services/apiGolService";

export default function App() {
  const svc = useMemo(() => new ApiGolService(60, 40, conway()), []);
  const [board, setBoard] = useState(() => createBoard(60, 40));
  const [initialized, setInitialized] = useState(false);

  const initialCells = useMemo(
    () => [
      { x: 2, y: 1 },
      { x: 3, y: 2 },
      { x: 1, y: 3 },
      { x: 2, y: 3 },
      { x: 3, y: 3 },
    ],
    []
  );

  useEffect(() => {
    let mounted = true;
    (async () => {
      const b = await svc.init(initialCells);
      if (mounted) {
        setBoard({ ...b });
        setInitialized(true);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [svc, initialCells]);

  const [playing, setPlaying] = useState(false);
  const [speedMs, setSpeedMs] = useState(120);
  const [stepsN] = useState(100);
  const [rule, setRule] = useState(svc.getRule().notation);
  const [ruleError, setRuleError] = useState<string | null>(null);

  useInterval(
    () => {
      (async () => {
        const next = await svc.next(true);
        setBoard({ ...next });
      })();
    },
    playing ? speedMs : null,
    playing && initialized
  );

  const handleRuleChange = (text: string) => {
    setRule(text);
    try {
      svc.setRule(text);
      setRuleError(null);
    } catch (e: any) {
      setRuleError(e.message);
    }
  };

  return (
    <div className="app">
      <div className="header">
        <h1>Conway’s Game of Life</h1>
        <div className="sub">
          Gen: {board.generation} • Size: {board.width}×{board.height} • Rule:{" "}
          {svc.getRule().notation}
          {ruleError ? ` • Rule error: ${ruleError}` : ""}
        </div>
      </div>

      <div className="layout">
        <CanvasBoard
          board={board}
          cellSize={14}
          grid
          onToggle={(x, y) => {
            const cur = svc.toggleCell(x, y);
            setBoard({ ...cur });
          }}
          onPaint={(x, y, alive) => {
            const cur = svc.setCell(x, y, alive);
            setBoard({ ...cur });
          }}
        />

        <ControlPanel
          playing={playing}
          speedMs={speedMs}
          steps={stepsN}
          rule={rule}
          onPlay={() => setPlaying(true)}
          onPause={() => setPlaying(false)}
          onNext={async () => {
            const b = await svc.next(true);
            setBoard({ ...b });
          }}
          onAdvance={async (n) => {
            const b = await svc.advance(n, true);
            setBoard({ ...b });
          }}
          onClear={() => {
            setPlaying(false);
            setBoard({ ...svc.clear() });
          }}
          onRandomize={() => {
            setPlaying(false);
            setBoard({ ...svc.randomize(0.25) });
          }}
          onSpeedChange={setSpeedMs}
          onRuleChange={handleRuleChange}
        />
      </div>
    </div>
  );
}
