import Header from "../../components/header/header";
import styles from "../../styles/resources.module.scss";
import Link from "next/link";
import { useAppSelector } from "../../app/hooks";
import { useRouter } from "next/router";

const Resources = () => {
	const auth = useAppSelector((s) => s.authSlice);
	const router = useRouter();

	if (!auth.isAuth || auth.role != 1) {
		router.replace("/");
	}

	return (
		<div className="mainContainer">
			<Header text="Resources" icon="Resources"></Header>
			<div className={styles.container}>
				<Link href="/resources/groups">
					<a className={styles.resource}>Groups</a>
				</Link>
				<Link href="/resources/users">
					<a className={styles.resource}>Users</a>
				</Link>
				{/*
					<Link href="/resources/teams">
						<a className={styles.resource}>Teams</a>
					</Link>
				*/}
				<Link href="/resources/sections">
					<a className={styles.resource}>Sections</a>
				</Link>
			</div>
		</div>
	);
};

export default Resources;
