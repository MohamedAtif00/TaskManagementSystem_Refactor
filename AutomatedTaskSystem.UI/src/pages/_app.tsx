import type { AppProps } from "next/app";
import Sidebar from "../components/sidebar/sidebar";
import "../styles/reset.css";
import "../styles/font.css";
import "../styles/index.scss";
import "../styles/index.css";
import { Provider } from "react-redux";
import { store } from "../app/store";
import Auth from "../components/auth";

const MyApp = ({ Component, pageProps }: AppProps) => {
	return (
		<Provider store={store}>
			<Auth>
				<div id="app">
					<Sidebar />
					<Component {...pageProps} />
				</div>
			</Auth>
		</Provider>
	);
}

export default MyApp;
