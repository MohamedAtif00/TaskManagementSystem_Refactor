import styles from "./styles.module.scss";

const Loader = () => {
	return <div className={[styles.loader, "border-slate-500"].join(" ")}></div>;
}

export default Loader;
