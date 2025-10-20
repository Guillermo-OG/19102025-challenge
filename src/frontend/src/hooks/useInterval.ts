import { useEffect, useRef } from "react";

export function useInterval(
  callback: () => void,
  delayMs: number | null,
  enabled = true
) {
  const saved = useRef(callback);
  useEffect(() => {
    saved.current = callback;
  }, [callback]);
  useEffect(() => {
    if (delayMs === null || !enabled) return;
    const id = setInterval(() => saved.current(), delayMs);
    return () => clearInterval(id);
  }, [delayMs, enabled]);
}
