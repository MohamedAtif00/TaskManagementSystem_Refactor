import Head  from "next/head"
import Link from "next/link"
import ResourcesIcon from "../../../assets/Icons/Resources"


export default function MembersLeaves(){



    return(
        <>
            <Head>
            <title>ATS - Members Leaves</title>
            </Head>
            <div className="mx-auto relative max-h-screen overflow-y-auto pr-4">
                <div className="bg-white border-solid border border-gray-300 rounded-b-md px-8 z-10 h-20 sticky top-0 left-0 right-0 flex items-center justify-between">
                    <div className="flex gap-2 items-center">
                        <div className="basis-6 h-6">
                            <ResourcesIcon />
                        </div>
                        <h1 className="font-bold text-2xl ">Users</h1>
                    </div>

                </div>
            </div>
        </>
    
    )
}