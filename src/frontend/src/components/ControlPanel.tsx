import React from "react";
import clsx from "clsx";

type Props = {
  playing: boolean;
  speedMs: number;
  steps: number;
  rule: string;

  onPlay: () => void;
  onPause: () => void;
  onNext: () => void;
  onAdvance: (steps: number) => void;
  onClear: () => void;
  onRandomize: () => void;
  onSpeedChange: (ms: number) => void;
  onRuleChange: (rule: string) => void;
};

export default function ControlPanel({
  playing,
  speedMs,
  steps,
  rule,
  onPlay,
  onPause,
  onNext,
  onAdvance,
  onClear,
  onRandomize,
  onSpeedChange,
  onRuleChange,
}: Props) {
  return (
    <div className="panel">
      <div className="row">
        <button
          className={clsx("btn", !playing && "primary")}
          onClick={onNext}
          title="Advance one generation"
        >
          Next
        </button>
        {playing ? (
          <button className="btn danger" onClick={onPause}>
            Pause
          </button>
        ) : (
          <button className="btn success" onClick={onPlay}>
            Play
          </button>
        )}
        <button className="btn" onClick={onClear}>
          Clear
        </button>
        <button className="btn" onClick={onRandomize}>
          Random
        </button>
      </div>

      <div className="row">
        <label className="label">
          Speed: {speedMs}ms
          <input
            type="range"
            min={30}
            max={800}
            value={speedMs}
            onChange={(e) => onSpeedChange(Number(e.target.value))}
          />
        </label>

        <label className="label">
          Advance N:
          <input
            type="number"
            min={1}
            max={10000}
            defaultValue={steps}
            onChange={() => {
              /* noop; controlled via button click param */
            }}
          />
          <button className="btn" onClick={() => onAdvance(steps)}>
            Advance {steps}
          </button>
        </label>

        <label className="label">
          Rule (B../S..):
          <input
            className="rule"
            value={rule}
            onChange={(e) => onRuleChange(e.target.value)}
            placeholder="B3/S23"
          />
        </label>
      </div>
    </div>
  );
}
