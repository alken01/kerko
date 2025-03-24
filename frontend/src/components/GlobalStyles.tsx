export function GlobalStyles() {
  return (
    <style jsx global>{`
      body {
        overscroll-behavior: none;
        touch-action: manipulation;
        -webkit-tap-highlight-color: transparent;
      }
      @supports (-webkit-touch-callout: none) {
        .min-h-screen {
          min-height: -webkit-fill-available;
        }
      }
      ::-webkit-scrollbar {
        display: none;
      }
      * {
        -ms-overflow-style: none;
        scrollbar-width: none;
      }
    `}</style>
  );
}
