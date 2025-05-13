import { useRouter } from "next/router";

interface UserNameLinkProps {
  userId: number;
  userName: string;
  className?: string;
}

const UserNameLink = ({ userId, userName, className = "" }: UserNameLinkProps) => {
  const router = useRouter();

  return (
    <span
      className={`cursor-pointer hover:text-blue-600 ${className}`}
      onClick={(e) => {
        e.preventDefault();
        e.stopPropagation();
        router.push(`/resources/users/${userId}`);
      }}
    >
      {userName}
    </span>
  );
};

export default UserNameLink; 