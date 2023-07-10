import type { AppProps } from "next/app";
import Sidebar from "../components/sidebar/sidebar";
import "../styles/reset.css";
import "../styles/index.scss";
import "../styles/index.css";
import { Provider } from "react-redux";
import { store } from "../app/store";
import Auth from "../components/auth";
import { Inter } from "next/font/google";

const inter = Inter({
    weight: ["400", "500", "700"],
    subsets: ["latin"],
});

const MyApp = ({ Component, pageProps }: AppProps) => {
    return (
        <Provider store={store}>
            <Auth>
                <div id="app" className={inter.className}>
                    <Sidebar />
                    <Component {...pageProps} />
                </div>
            </Auth>
        </Provider>
    );
};

export default MyApp;
