import type { AppProps } from "next/app";
import Sidebar from "../components/sidebar/sidebar";
import "../styles/reset.css";
import "../styles/index.scss";
import "../styles/index.css";
import { Provider } from "react-redux";
import { store } from "../app/store";
import Auth from "../components/auth";
import { Inter } from "next/font/google";
import { ToastContainer } from "react-toastify";
import 'react-toastify/dist/ReactToastify.css';
import SignalRProvider from "../components/connection/connectionProvider";


const inter = Inter({
    weight: ["400", "500", "700"],
    subsets: ["latin"],
});

const MyApp = ({ Component, pageProps }: AppProps) => {
    // const router = useRouter();
    



    return (
        <Provider store={store}>
            <Auth>
                <SignalRProvider >
                    <ToastContainer position="top-right" autoClose={10000} />
                    <div
                        id="app"
                        className={[inter.className, "bg-slate-200"].join(" ")}
                    >

                        <Sidebar />
                        <Component  {...pageProps} />
                    </div>
                </SignalRProvider>
            </Auth>
        </Provider>
    );
};

export default MyApp;
