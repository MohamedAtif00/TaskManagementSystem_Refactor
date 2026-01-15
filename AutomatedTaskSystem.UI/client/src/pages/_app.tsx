import type { AppProps } from "next/app";
import Head from "next/head";
import Sidebar from "../components/sidebar/sidebar";
import "../styles/reset.css";
import "../styles/index.scss";
import "../styles/index.css";
import { Provider } from "react-redux";
import { store } from "../app/store";
import Auth from "../components/auth";
import { ToastContainer } from "react-toastify";
import 'react-toastify/dist/ReactToastify.css';
import SignalRProvider from "../components/connection/connectionProvider";

const MyApp = ({ Component, pageProps }: AppProps) => {
    // const router = useRouter();
    



    return (
        <Provider store={store}>
            {/* <Head>
                <base href="/" />
            </Head> */}
            <Auth>
                <SignalRProvider >
                    <ToastContainer position="top-right" autoClose={10000} />
                    <div
                        id="app"
                        className="bg-slate-200 font-sans"
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
