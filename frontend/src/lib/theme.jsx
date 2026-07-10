import { createContext, useContext, useEffect, useState } from "react";

const ThemeContext = createContext({
  theme: "light",
  setTheme: () => {},
});

export function ThemeProvider({ children }) {
  const [theme, setThemeState] = useState("light");

  useEffect(() => {
    const stored = localStorage.getItem("srm-theme");
    if (stored === "dark" || stored === "light") {
      setThemeState(stored);
      document.documentElement.classList.toggle("dark", stored === "dark");
    }
  }, []);

  const setTheme = (t) => {
    setThemeState(t);
    localStorage.setItem("srm-theme", t);
    document.documentElement.classList.toggle("dark", t === "dark");
  };

  return <ThemeContext.Provider value={{ theme, setTheme }}>{children}</ThemeContext.Provider>;
}

export const useTheme = () => useContext(ThemeContext);
